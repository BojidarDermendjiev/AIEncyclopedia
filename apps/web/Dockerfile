# Production multi-stage build
FROM node:20-alpine AS build
WORKDIR /app

# Copy workspace files
COPY package*.json ./
COPY packages/shared/package*.json ./packages/shared/
COPY apps/web/package*.json ./apps/web/

# We're building just the web app, copy its source
COPY packages/shared/ ./packages/shared/
COPY apps/web/ ./apps/web/

RUN npm ci --workspace=apps/web --workspace=packages/shared
RUN npm run build --workspace=packages/shared
RUN npm run build --workspace=apps/web

FROM nginx:alpine AS runtime
COPY --from=build /app/apps/web/dist /usr/share/nginx/html
COPY apps/web/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
