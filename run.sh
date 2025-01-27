#!/bin/bash

# Load environment variables from .env file
export $(grep -v '^#' .env | xargs)

# Trim the ENABLE_SSL value in case of extra whitespace or newlines
ENABLE_SSL=$(echo "$ENABLE_SSL" | tr -d '\r' | tr -d '[:space:]')

# Check the value of ENABLE_SSL
if [ "$ENABLE_SSL" = "true" ]; then
  echo "SSL is enabled, running docker-compose with SSL..."
  docker-compose -f docker-compose.yml -f docker-compose.ssl.yml up --build -d
else
  echo "SSL is not enabled, running docker-compose without SSL..."
  docker-compose -f docker-compose.yml up --build -d
fi

echo "Containers are running in the background."
