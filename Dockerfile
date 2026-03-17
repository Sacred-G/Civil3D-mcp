FROM node:20-alpine

WORKDIR /app

COPY package*.json ./
RUN npm ci --omit=dev

COPY build/ ./build/

EXPOSE 3000

ENV NODE_ENV=production

ENTRYPOINT ["node", "build/index.js"]
