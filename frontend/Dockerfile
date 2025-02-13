# Stage 1: Build the Angular application
FROM node:18-alpine AS build

WORKDIR /app

# Copy package.json and package-lock.json
COPY package*.json ./

# Install dependencies
RUN npm install --legacy-peer-deps

# Copy the rest of the application code
COPY . .

# Build the Angular app for production
RUN npm run build -- --configuration production

# Stage 2: Serve the Angular application using Nginx
FROM nginx:1.27.1-alpine

# Install gettext for envsubst
RUN apk add --no-cache gettext

# Copy the build output to Nginx's default directory
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html

# Copy the Nginx configuration file
COPY nginx.conf /etc/nginx/nginx.conf

# Copy the default config
COPY default.conf /etc/nginx/conf.d/default.conf

# Expose port 80 and 443
EXPOSE 80
EXPOSE 443

# Start Nginx
CMD ["nginx", "-g", "daemon off;"]
