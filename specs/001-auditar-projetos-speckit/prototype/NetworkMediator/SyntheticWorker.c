#define WIN32_LEAN_AND_MEAN
#define _WIN32_WINNT 0x0A00
#include <winsock2.h>
#include <ws2tcpip.h>
#include <windows.h>
#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <wchar.h>

static void output(const char* text) { DWORD count=0; WriteFile(GetStdHandle(STD_OUTPUT_HANDLE),text,(DWORD)strlen(text),&count,NULL); }
static BOOL token_state(BOOL* app,BOOL* admin) {
    HANDLE token=NULL;DWORD size=0;BOOL ok=FALSE;PSID adminSid=NULL;SID_IDENTIFIER_AUTHORITY nt=SECURITY_NT_AUTHORITY;
    *app=FALSE;*admin=TRUE;
    if(!OpenProcessToken(GetCurrentProcess(),TOKEN_QUERY,&token))return FALSE;
    if(!GetTokenInformation(token,TokenIsAppContainer,app,sizeof(*app),&size))goto done;
    GetTokenInformation(token,TokenGroups,NULL,0,&size);
    TOKEN_GROUPS* groups=(TOKEN_GROUPS*)HeapAlloc(GetProcessHeap(),0,size);
    if(!groups)goto done;
    if(GetTokenInformation(token,TokenGroups,groups,size,&size)&&AllocateAndInitializeSid(&nt,2,SECURITY_BUILTIN_DOMAIN_RID,DOMAIN_ALIAS_RID_ADMINS,0,0,0,0,0,0,&adminSid)) {
        *admin=FALSE;for(DWORD i=0;i<groups->GroupCount;i++)if(EqualSid(groups->Groups[i].Sid,adminSid))*admin=(groups->Groups[i].Attributes&SE_GROUP_ENABLED)!=0;
        ok=TRUE;
    }
    HeapFree(GetProcessHeap(),0,groups);
done:
    if(adminSid)FreeSid(adminSid);if(token)CloseHandle(token);return ok;
}
static BOOL denied_access(const wchar_t* path,DWORD access) {
    HANDLE handle=CreateFileW(path,access,FILE_SHARE_READ|FILE_SHARE_WRITE|FILE_SHARE_DELETE,NULL,OPEN_EXISTING,FILE_ATTRIBUTE_NORMAL,NULL);
    if(handle!=INVALID_HANDLE_VALUE){CloseHandle(handle);return FALSE;}
    return GetLastError()==ERROR_ACCESS_DENIED;
}
static BOOL tcp_probe(BOOL ipv6,unsigned short port,int* error) {
    SOCKET sock=socket(ipv6?AF_INET6:AF_INET,SOCK_STREAM,IPPROTO_TCP);*error=0;if(sock==INVALID_SOCKET){*error=WSAGetLastError();return FALSE;}
    u_long one=1;if(ioctlsocket(sock,FIONBIO,&one)!=0){*error=WSAGetLastError();closesocket(sock);return FALSE;}
    int status;
    if(ipv6){struct sockaddr_in6 address={0};address.sin6_family=AF_INET6;address.sin6_port=htons(port);address.sin6_addr=in6addr_loopback;status=connect(sock,(struct sockaddr*)&address,sizeof(address));}
    else{struct sockaddr_in address={0};address.sin_family=AF_INET;address.sin_port=htons(port);address.sin_addr.s_addr=htonl(INADDR_LOOPBACK);status=connect(sock,(struct sockaddr*)&address,sizeof(address));}
    BOOL connected=status==0;
    if(!connected){int first=WSAGetLastError();if(first==WSAEWOULDBLOCK||first==WSAEINPROGRESS){fd_set writeSet,errorSet;FD_ZERO(&writeSet);FD_ZERO(&errorSet);FD_SET(sock,&writeSet);FD_SET(sock,&errorSet);struct timeval wait={1,500000};status=select(0,NULL,&writeSet,&errorSet,&wait);if(status>0){int final=0,length=sizeof(final);if(getsockopt(sock,SOL_SOCKET,SO_ERROR,(char*)&final,&length)==0){connected=final==0&&FD_ISSET(sock,&writeSet);*error=final;}else *error=WSAGetLastError();}else *error=status==0?WSAETIMEDOUT:WSAGetLastError();}else *error=first;}
    closesocket(sock);return connected;
}
static BOOL append_arg(wchar_t* target,size_t capacity,size_t* offset,const wchar_t* value) {
    if(*offset+2>=capacity)return FALSE;target[(*offset)++]=L'"';
    size_t slashes=0;
    for(const wchar_t* at=value;;at++){
        if(*at==L'\\'){slashes++;continue;}
        size_t copies=(*at==L'"'||*at==0)?slashes*2:slashes;
        if(*at==L'"')copies++;
        while(copies--){if(*offset+2>=capacity)return FALSE;target[(*offset)++]=L'\\';}
        slashes=0;if(*at==0)break;if(*offset+2>=capacity)return FALSE;target[(*offset)++]=*at;
    }
    target[(*offset)++]=L'"';target[*offset]=0;return TRUE;
}
static int launch_child(int argc,wchar_t** argv) {
    wchar_t executable[32768],command[32768];size_t offset=0;DWORD size=GetModuleFileNameW(NULL,executable,32768);if(size==0||size>=32768)return 80;
    if(!append_arg(command,32768,&offset,executable))return 81;
    for(int i=1;i<argc;i++){if(offset+1>=32768)return 81;command[offset++]=L' ';command[offset]=0;if(!append_arg(command,32768,&offset,argc==2?L"--bootstrap-check":(i==8?L"child":argv[i])))return 81;}
    HANDLE handles[2]={GetStdHandle(STD_OUTPUT_HANDLE),argc==9?(HANDLE)(ULONG_PTR)_wcstoui64(argv[2],NULL,10):NULL};
    for(int i=0;i<(argc==9?2:1);i++){if(!SetHandleInformation(handles[i],HANDLE_FLAG_INHERIT,HANDLE_FLAG_INHERIT))return 85;}
    SIZE_T bytes=0;InitializeProcThreadAttributeList(NULL,1,0,&bytes);LPPROC_THREAD_ATTRIBUTE_LIST attributes=(LPPROC_THREAD_ATTRIBUTE_LIST)HeapAlloc(GetProcessHeap(),0,bytes);if(!attributes)return 82;
    int result=83;BOOL initialized=InitializeProcThreadAttributeList(attributes,1,0,&bytes);
    if(initialized&&UpdateProcThreadAttribute(attributes,0,PROC_THREAD_ATTRIBUTE_HANDLE_LIST,handles,argc==9?sizeof(handles):sizeof(HANDLE),NULL,NULL)){
        STARTUPINFOEXW startup={0};startup.StartupInfo.cb=sizeof(startup);startup.StartupInfo.dwFlags=STARTF_USESTDHANDLES;startup.StartupInfo.hStdOutput=handles[0];startup.StartupInfo.hStdError=handles[0];startup.lpAttributeList=attributes;PROCESS_INFORMATION child={0};
        if(CreateProcessW(executable,command,NULL,NULL,TRUE,EXTENDED_STARTUPINFO_PRESENT|CREATE_NO_WINDOW,NULL,NULL,&startup.StartupInfo,&child)){
            DWORD wait=WaitForSingleObject(child.hProcess,10000),exitCode=84;
            if(wait!=WAIT_OBJECT_0){TerminateProcess(child.hProcess,84);WaitForSingleObject(child.hProcess,2000);}GetExitCodeProcess(child.hProcess,&exitCode);result=(int)exitCode;CloseHandle(child.hThread);CloseHandle(child.hProcess);
        }
    }
    if(result==83){char diagnostic[128];sprintf_s(diagnostic,sizeof(diagnostic),"{\"Event\":\"ChildLaunchFailed\",\"Win32Error\":%lu}\n",GetLastError());output(diagnostic);}
    if(initialized)DeleteProcThreadAttributeList(attributes);HeapFree(GetProcessHeap(),0,attributes);return result;
}
int wmain(int argc,wchar_t** argv) {
    BOOL app=FALSE,admin=TRUE;BOOL known=token_state(&app,&admin);char line[2048];
    if(argc==2&&(wcscmp(argv[1],L"--bootstrap-check")==0||wcscmp(argv[1],L"--bootstrap-tree")==0)){sprintf_s(line,sizeof(line),"{\"Bootstrap\":\"READY\",\"ProcessId\":%lu,\"TokenKnown\":%s,\"IsAppContainer\":%s,\"AdminGroupEnabled\":%s}\n",GetCurrentProcessId(),known?"true":"false",app?"true":"false",admin?"true":"false");output(line);if(!known)return 60;return wcscmp(argv[1],L"--bootstrap-tree")==0?launch_child(argc,argv):0;}
    if(argc==2&&wcscmp(argv[1],L"--imports-check")==0){output("NATIVE_SYNTHETIC_WORKER_NO_SHELL\n");return 0;}
    if(argc!=9||wcscmp(argv[1],L"--prove")!=0)return 64;
    if(!known||!app||admin)return 65;BOOL inJob=FALSE;if(!IsProcessInJob(GetCurrentProcess(),NULL,&inJob)||!inJob)return 66;
    if(wcscmp(argv[8],L"root")!=0&&wcscmp(argv[8],L"child")!=0)return 67;
    unsigned short ports[4];for(int i=0;i<4;i++){wchar_t* end=NULL;unsigned long port=wcstoul(argv[i+4],&end,10);if(!end||*end||port==0||port>65535)return 68;ports[i]=(unsigned short)port;}
    const wchar_t* path=argv[3];size_t length=wcslen(path);if(length<10||wcscmp(path+length-10,L"source.txt")!=0)return 69;
    HANDLE source=(HANDLE)(ULONG_PTR)_wcstoui64(argv[2],NULL,10);if(source==NULL||source==INVALID_HANDLE_VALUE)return 70;
    sprintf_s(line,sizeof(line),"{\"Event\":\"WorkerReady\",\"Pid\":%lu}\n",GetCurrentProcessId());output(line);
    LARGE_INTEGER zero={0};char content[128]={0};DWORD read=0;BOOL readOk=SetFilePointerEx(source,zero,NULL,FILE_BEGIN)&&ReadFile(source,content,sizeof(content)-1,&read,NULL);BOOL matches=readOk&&strcmp(content,"synthetic-read-only-source")==0;
    BOOL writeDenied=denied_access(path,GENERIC_WRITE),deleteDenied=denied_access(path,DELETE),renameDenied=deleteDenied;
    wchar_t newPath[32768];if(swprintf_s(newPath,32768,L"%s.new",path)<0)return 71;
    HANDLE created=CreateFileW(newPath,GENERIC_WRITE,FILE_SHARE_READ,NULL,CREATE_NEW,FILE_ATTRIBUTE_NORMAL,NULL);BOOL createDenied=FALSE;if(created==INVALID_HANDLE_VALUE)createDenied=GetLastError()==ERROR_ACCESS_DENIED;else CloseHandle(created);
    WSADATA data;int wsa=WSAStartup(MAKEWORD(2,2),&data),allowError=0,denyError=0,ipv6Error=0,udpError=0;BOOL allowed=FALSE,denied=FALSE,ipv6=FALSE;
    if(wsa==0){allowed=tcp_probe(FALSE,ports[0],&allowError);denied=tcp_probe(FALSE,ports[1],&denyError);ipv6=tcp_probe(TRUE,ports[2],&ipv6Error);SOCKET u=socket(AF_INET,SOCK_DGRAM,IPPROTO_UDP);if(u!=INVALID_SOCKET){struct sockaddr_in address={0};address.sin_family=AF_INET;address.sin_port=htons(ports[3]);address.sin_addr.s_addr=htonl(INADDR_LOOPBACK);if(sendto(u,"SYNTHETIC",9,0,(struct sockaddr*)&address,sizeof(address))==SOCKET_ERROR)udpError=WSAGetLastError();closesocket(u);}else udpError=WSAGetLastError();WSACleanup();}
    sprintf_s(line,sizeof(line),"{\"Role\":\"%s\",\"ReadViaBroker\":%s,\"WriteDenied\":%s,\"CreateDenied\":%s,\"RenameDenied\":%s,\"DeleteDenied\":%s,\"AllowedConnected\":%s,\"DeniedConnected\":%s,\"Ipv6Connected\":%s,\"WsaStartupError\":%d,\"AllowError\":%d,\"DenyError\":%d,\"Ipv6Error\":%d,\"UdpError\":%d,\"IsAppContainer\":true,\"AdminGroupEnabled\":false}\n",wcscmp(argv[8],L"root")==0?"root":"child",matches?"true":"false",writeDenied?"true":"false",createDenied?"true":"false",renameDenied?"true":"false",deleteDenied?"true":"false",allowed?"true":"false",denied?"true":"false",ipv6?"true":"false",wsa,allowError,denyError,ipv6Error,udpError);output(line);
    if(wcscmp(argv[8],L"root")==0)return launch_child(argc,argv);return 0;
}