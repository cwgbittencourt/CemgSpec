using System.Net;
using System.Net.Sockets;
using SpecAudit.Domain;

namespace SpecAudit.Infrastructure.Documents;

internal sealed class RemotePreviewImages(Func<DnsEndPoint, CancellationToken, ValueTask<Stream>> connect)
{
    private const int MaxBytes = 4 * 1024 * 1024;
    internal static RemotePreviewImages Default { get; } = new(ConnectPublicAsync);

    internal async Task<PreviewRasterImage> LoadAsync(Uri uri, CancellationToken cancellationToken)
    {
        if (uri.Scheme is not ("http" or "https") || !uri.IsDefaultPort || uri.UserInfo.Length != 0 ||
            (IPAddress.TryParse(uri.IdnHost.Trim('[', ']'), out var ip) && !IsPublic(ip)))
            throw new DocumentAccessException("OutOfScope");

        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(10));
        using var client = new HttpClient(new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false,
            UseProxy = false,
            Credentials = null,
            ConnectCallback = (context, token) => connect(context.DnsEndPoint, token)
        });
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        try
        {
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, deadline.Token);
            var mediaType = response.Content.Headers.ContentType?.MediaType?.ToLowerInvariant();
            if (!response.IsSuccessStatusCode || mediaType is not ("image/png" or "image/jpeg" or "image/webp") ||
                response.Content.Headers.ContentLength > MaxBytes)
                throw new DocumentAccessException("RemoteImageUnavailable");

            using var stream = await response.Content.ReadAsStreamAsync(deadline.Token);
            using var bytes = new MemoryStream();
            var buffer = new byte[8192];
            int read;
            while ((read = await stream.ReadAsync(buffer, deadline.Token)) != 0)
            {
                if (bytes.Length + read > MaxBytes)
                    throw new DocumentAccessException("RemoteImageUnavailable");
                bytes.Write(buffer, 0, read);
            }
            var raster = PreviewRasterImage.FromSnapshot(DocumentSnapshot.Create(Guid.NewGuid(), bytes.ToArray(), 0));
            if (raster is null || !raster.DataUri.StartsWith($"data:{mediaType};", StringComparison.Ordinal))
                throw new DocumentAccessException("RemoteImageUnavailable");
            return raster;
        }
        catch (Exception error) when (error is HttpRequestException or IOException ||
                                      error is OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            throw new DocumentAccessException("RemoteImageUnavailable");
        }
    }

    private static async ValueTask<Stream> ConnectPublicAsync(DnsEndPoint endpoint, CancellationToken cancellationToken)
    {
        var addresses = await Dns.GetHostAddressesAsync(endpoint.Host, cancellationToken);
        foreach (var address in addresses.Where(IsPublic))
        {
            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await socket.ConnectAsync(new IPEndPoint(address, endpoint.Port), cancellationToken);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch (SocketException)
            {
                socket.Dispose();
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }
        throw new HttpRequestException("Public image endpoint unavailable.");
    }

    internal static bool IsPublic(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();
        var bytes = address.GetAddressBytes();
        if (bytes.Length == 4)
        {
            var (first, second, third) = (bytes[0], bytes[1], bytes[2]);
            return first is > 0 and < 224 and not (10 or 127) &&
                !(first == 100 && second is >= 64 and <= 127) &&
                !(first == 169 && second == 254) &&
                !(first == 172 && second is >= 16 and <= 31) &&
                !(first == 192 && (second == 168 || second == 0 && third is 0 or 2 || second == 88 && third == 99)) &&
                !(first == 198 && (second is 18 or 19 || second == 51 && third == 100)) &&
                !(first == 203 && second == 0 && third == 113);
        }
        return bytes[0] is >= 0x20 and <= 0x3f &&
            !(bytes[0] == 0x20 && (bytes[1] == 2 || bytes[1] == 1 && (bytes[2] < 2 || bytes[2] == 0x0d && bytes[3] == 0xb8))) &&
            !(bytes[0] == 0x3f && bytes[1] == 0xff);
    }
}
