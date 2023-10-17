# Weaviate / Qdrant Deployment

This document provides a quick guide on setting up [Weaviate](https://weaviate.io/) and [Qdrant](https://qdrant.tech/) with Docker Compose. Follow the steps below to ensure a smooth setup.

## Preparation Steps

1. **Create Data Folder**:
   Before running `docker-compose up`, create folders named `weaviate_data` and `qdrant_data` in the same directory as your `docker-compose.yml` file.

```bash
mkdir weaviate_data
mkdir qdrant_data
```

2. **Rename Environment File**:
   Rename the file `.env.example` to `.env`.

```bash
mv .env.example .env
```

3. **Update API Key**:
   Open the `.env` file and replace the placeholder text with your actual `OPENAI_APIKEY`.

```bash
OPENAI_APIKEY=your_api_key_here
```

## Deployment

Now you are ready to deploy Weaviate and Qdrant using Docker Compose. The `profiles` option in the `docker-compose.yml` file is crucial for controlling which services are started when you run Docker Compose. Each service in the `docker-compose.yml` file has a `profiles` property which contains the profiles under which the service should be run.

1. **Running Weaviate**:
   To start the Weaviate service, use the following command:

```bash
docker compose --profile weaviate up
```

2. **Running Qdrant**:
   To start the Qdrant service, use the following command:

```bash
docker compose --profile qdrant up
```

3. **Running Both Services**:
   If you want to run both Weaviate and Qdrant services at the same time, you can specify both profiles in the command:

```bash
docker compose --profile weaviate --profile qdrant up
```

This way, you can control which services are started and run them either individually or together based on the profiles specified in the `docker-compose.yml` file.
