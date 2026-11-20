using SpecAudit.Infrastructure.Documents;
using SpecAudit.Infrastructure.Execution;

namespace SpecAudit.Components.Documents;

public static class DocumentMessages
{
    public static bool IsReadFailure(Exception failure) =>
        failure is DocumentAccessException or LimitExceededException or IOException or UnauthorizedAccessException
            or KeyNotFoundException or InvalidOperationException;

    public static string For(Exception failure) => failure switch
    {
        DocumentAccessException access => For(access.Code),
        LimitExceededException => "O documento excede um limite de leitura permitido e não foi exibido.",
        KeyNotFoundException => "O projeto não pertence a esta sessão.",
        _ => "Não foi possível ler o documento. Confira a pasta e tente novamente."
    };

    public static string For(string code) => code switch
    {
        "Changed" => "Os documentos mudaram desde a leitura. Use Atualizar documentos e abra o documento novamente.",
        "OutOfScope" => "Referência fora do catálogo autorizado. O destino não foi aberto.",
        "InvalidEncoding" => "O documento não está em UTF-8 válido e não pode ser exibido.",
        "PreviewNestingLimit" => "O documento excede o limite de aninhamento do preview.",
        "RemoteImageUnavailable" => "Não foi possível carregar a imagem remota. Endereços locais ou privados, redirecionamentos, tipos diferentes de PNG/JPEG/WebP e arquivos acima de 4 MiB são recusados.",
        "Unreadable" => "Não foi possível ler o documento. Confira a existência e as permissões.",
        _ => "Não foi possível abrir o documento."
    };
}

