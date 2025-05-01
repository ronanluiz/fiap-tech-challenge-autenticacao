#!/bin/bash

# Criação do bucket S3 para manutenção se estado dos recursos
BUCKET_NAME="tech-challenge-soat10-fase3"
REGION="us-east-1"

if aws s3api head-bucket --bucket $BUCKET_NAME --region $REGION 2>/dev/null; then
    echo "O bucket S3 '$BUCKET_NAME' já existe na região '$REGION'."
else
    echo "O bucket S3 '$BUCKET_NAME' não existe na região '$REGION'. Criando..."

    # Criar o bucket S3
    aws s3api create-bucket --bucket $BUCKET_NAME --region $REGION

    # Verifique se o bucket foi criado com sucesso
    if [ $? -eq 0 ]; then
        echo "Bucket $BUCKET_NAME criado com sucesso."
    else
        echo "Falha ao criar o bucket."
        exit 1
    fi
fi