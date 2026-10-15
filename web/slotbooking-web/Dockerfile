# Build context is the repo root (see docker-compose.yml).
FROM node:22-alpine AS build
WORKDIR /src
COPY web/slotbooking-web/package*.json ./
RUN npm ci --no-audit --no-fund
COPY web/slotbooking-web/ ./
RUN npx ng build

FROM nginx:1.27-alpine AS final
COPY web/slotbooking-web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /src/dist/investmentorders-web/browser /usr/share/nginx/html
EXPOSE 80
