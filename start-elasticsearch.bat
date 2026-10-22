@echo off
title Elasticsearch Search Server
echo Starting Elasticsearch Server using System Java...

rem Force Elasticsearch to use Adoptium OpenJDK 21
set "ES_JAVA_HOME=C:\Program Files\Eclipse Adoptium\jdk-21.0.2.13-hotspot"

rem Switch both drive and folder to C:\elasticsearch\bin
cd /d C:\elasticsearch\bin
elasticsearch.bat
pause
