#!/usr/bin/env bash
set -euo pipefail

[ -f "$HOME/.bash_profile" ] && . "$HOME/.bash_profile"

if [ -f /minitwit/.env ]; then
  set -a
  . /minitwit/.env
  set +a
fi

if [[ -n "${DOCKER_USERNAME:-}" && -n "${DOCKER_PASSWORD:-}" ]]; then
  echo "$DOCKER_PASSWORD" | docker login -u "$DOCKER_USERNAME" --password-stdin || true
fi

COMPOSE="/minitwit/docker-compose.yml"

docker compose -f "$COMPOSE" pull
docker compose -f "$COMPOSE" up -d
docker image prune -f >/dev/null 2>&1 || true
