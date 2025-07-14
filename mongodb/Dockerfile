# Base image for the build stage
FROM alpine:latest AS builder
WORKDIR /app
COPY mongodb-init ./
RUN apk add --no-cache openssl


FROM mongo:latest

# Copy initialization scripts from the build stage
COPY --from=builder /app /docker-entrypoint-initdb.d

# Set the environment variable
ENV MONGO_INITDB_DATABASE=OrdersDatabase

# Expose the MongoDB port
EXPOSE 27017