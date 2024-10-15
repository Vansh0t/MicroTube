# MicroTube
[![Test, Build & Deploy API](https://github.com/Vansh0t/MicroTube/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Vansh0t/MicroTube/actions/workflows/dotnet.yml) [![Build & Deploy Angular Client](https://github.com/Vansh0t/MicroTube/actions/workflows/angular.yml/badge.svg?branch=master)](https://github.com/Vansh0t/MicroTube/actions/workflows/angular.yml)

MicroTube is a video hosting and streaming service made with [.NET](https://dotnet.microsoft.com) and [Angular](https://angular.dev). The out-of-the-box version of the project uses [SqlServer](https://www.microsoft.com/sql-server) as a database and some [Azure](https://azure.microsoft.com/) services, but they can be easily switched out. Preview production version is available at https://microtube.dev.
# Features
- Integrated complete authentication system featuring high security and session management
- Robust video uploading and processing pipeline made with [Hangfire](https://www.hangfire.io) and [FFmpeg](https://www.ffmpeg.org)
- Extensive search with typing suggestions, sorting and filtering made with [Elasticsearch](https://www.elastic.co/docs)
- [AzureCDN](https://azure.microsoft.com/products/cdn) for media content distribution
- Like/Dislike and Views tracking and aggregation
- Responsive UI, using [Angular Material](https://material.angular.io)
- Completely dockerized with docker-compose.yml
- Extensive configuration
- OpenAPI docs at https://api.microtube.dev/swagger
