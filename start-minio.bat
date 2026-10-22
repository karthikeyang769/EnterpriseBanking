@echo off
title MinIO Object Storage Server
echo Starting MinIO Server...
C:\minio\minio.exe server C:\minio_data --console-address :9001
pause
