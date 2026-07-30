# Stage 1: Build React static files
FROM node:18-alpine AS builder
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
ENV REACT_APP_API_BASE_URL=/api
ENV REACT_APP_AI_SERVICE_URL=
RUN npm run build

# Stage 2: Serve using Nginx web server
FROM nginx:alpine
COPY --from=builder /app/build /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
