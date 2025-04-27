FROM mcr.microsoft.com/mssql/server:2022-latest

# Zmienne środowiskowe
ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=YourStrong!Passw0rd
ENV MSSQL_PID=Developer

# Przełącz na root, żeby mieć uprawnienia
USER root

# Instalacja narzędzi i sqlpackage
RUN apt-get update && \
    apt-get install -y curl unzip libunwind8 && \
    curl -L -o sqlpackage.zip "https://aka.ms/sqlpackage-linux" && \
    unzip sqlpackage.zip -d /opt/sqlpackage && \
    chmod +x /opt/sqlpackage/sqlpackage && \
    ln -s /opt/sqlpackage/sqlpackage /usr/local/bin/sqlpackage && \
    rm sqlpackage.zip

# Stwórz katalog na plik dacpac
RUN mkdir -p /var/opt/mssql/dacpac

# Kopiuj plik dacpac
COPY ./bin/Debug/Database.dacpac /var/opt/mssql/dacpac/

# Po instalacji przełącz z powrotem na użytkownika mssql
USER mssql

# Komenda startowa
CMD /bin/bash -c "\
    /opt/mssql/bin/sqlservr & \
    sleep 20 && \
    echo 'Wdrażam plik .dacpac...' && \
    sqlpackage \
      /Action:Publish \
      /SourceFile:/var/opt/mssql/dacpac/Database.dacpac \
      /TargetConnectionString:\"Server=localhost;Database=PersonalFinanceManager;User Id=sa;Password=YourStrong!Passw0rd;Encrypt=False;TrustServerCertificate=True;\" \
      /p:BlockOnPossibleDataLoss=false && \
    wait"
