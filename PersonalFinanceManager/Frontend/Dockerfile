# Użyj oficjalnego obrazu Node.js
FROM node:20

# Utwórz katalog roboczy
WORKDIR /app

# Skopiuj package.json i zainstaluj zależności
COPY package*.json ./
RUN npm install

# Skopiuj resztę plików aplikacji
COPY . .

# Aplikacja działa np. na porcie 3000
EXPOSE 3000

# Domyślna komenda uruchamiająca serwer
CMD ["npm", "start"]
