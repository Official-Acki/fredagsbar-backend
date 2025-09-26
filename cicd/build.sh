#!/bin/bash

export REMOTE=registry.pawzd.net
export PROJECT=alcoholics/backend

docker login $REMOTE

# Version is current git hash
export VERSION=$(git rev-parse --short HEAD)
export BRANCH=$(git rev-parse --abbrev-ref HEAD)

IMAGE_EXISTS=$(docker images -q $REMOTE/$PROJECT:$VERSION)
if [ ! -n "$IMAGE_EXISTS" ]; then
    IMAGE_EXISTS=$(docker images -q $REMOTE/$PROJECT:$BRANCH-$VERSION)
fi
if [ -n "$IMAGE_EXISTS" ]; then
    echo "Image $REMOTE/$PROJECT:$VERSION already exists."
    read -p "Do you want to rebuild it? (y/N): " REBUILD_CONFIRM
fi
if [ "$REBUILD_CONFIRM" = "y" ] || [ "$REBUILD_CONFIRM" = "Y" ] || [ ! -n "$IMAGE_EXISTS" ]; then
    if [ $BRANCH = "main" ]; then
        docker build --no-cache \
            -t $REMOTE/$PROJECT:$VERSION \
            -t $REMOTE/$PROJECT:latest . --build-arg version=$VERSION
    else
        docker build \
            -t $REMOTE/$PROJECT:$BRANCH-$VERSION \
            -t $REMOTE/$PROJECT:$BRANCH-latest . --build-arg version=$BRANCH-latest
    fi
fi

# User input of Y/n if they wish to push
read -r -p "Do you wish to push the image(s) to $REMOTE? (Y/n) " PUSH_CONFIRM
if [ "$PUSH_CONFIRM" = "Y" ] || [ "$PUSH_CONFIRM" = "y" ] || [ -z "$PUSH_CONFIRM" ]; then
    if [ $BRANCH = "main" ]; then
        docker push $REMOTE/$PROJECT:$VERSION
        docker push $REMOTE/$PROJECT:latest
    else
        docker push $REMOTE/$PROJECT:$BRANCH-$VERSION
        docker push $REMOTE/$PROJECT:$BRANCH-latest
    fi
fi