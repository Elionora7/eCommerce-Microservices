# Base image for the build stage
FROM alpine:latest AS builder
WORKDIR /app
COPY postgres-init ./
RUN apk add --no-cache openssl

# Latest PostgreSQL image
FROM postgres:latest

# Copy initialization scripts from the build stage
COPY --from=builder /app /docker-entrypoint-initdb.d

# Set the Postgres environment variables
ENV POSTGRES_USER=postgres
ENV POSTGRES_PASSWORD=P@ssw0rd
ENV POSTGRES_DB=eCommerceUsers

# Expose the PostgreSQL port
EXPOSE 5432