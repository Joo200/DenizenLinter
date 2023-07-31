FROM mcr.microsoft.com/dotnet/runtime:6.0

ADD ./app/ /denizenLinter/

ENTRYPOINT [ "/denizenLinter/DenizenLinter" ]
