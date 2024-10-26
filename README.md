# MicroTube
[![Live badge](https://img.shields.io/badge/Live-microtube.dev-brightgreen.svg)](https://microtube.dev/) [![Test, Build & Deploy API](https://github.com/Vansh0t/MicroTube/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Vansh0t/MicroTube/actions/workflows/dotnet.yml) [![Build & Deploy Angular Client](https://github.com/Vansh0t/MicroTube/actions/workflows/angular.yml/badge.svg?branch=master)](https://github.com/Vansh0t/MicroTube/actions/workflows/angular.yml)

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
# Quickstart (Dev)
- Make sure you have a Docker version supporting "docker compose"
- Set the following secrets in a preferable way (I use secrets.json). The values in the example below should work out of the box. <b>REPLACE THEM WITH YOUR OWN FOR PRODUCTION.</b>
```json
{
	"ConnectionStrings": {
		"Default": "Server=tcp:sql-server,1433;Database=MicroTubeDb;User Id=sa;Password=devpassword12345++;Trusted_Connection=False;Connect Timeout=20;Encrypt=False;Trust Server Certificate=True;Command Timeout=10"
	},
	"JwtAccessTokens": {
		"Key": "some_64_bytestring_EyTF0Zk3iMJyphaTa3j9uEeYDVdxSzuwqCePduTuP9jPA"
	},
	"AuthenticationEmailing": {
		"SenderSMTPPassword": "no-reply-password"
	},
	"AzureBlobStorage": {
		"ConnectionString": "UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://azurite"
	},
	"ElasticSearch": {
		"ApiKey": "does_not_matter_in_development"
	}
}
```
 - Run "docker compose up".
 - Trust the localhost certificate in Shared/Dev_Certificates/ to remove https warning in browsers.
 - Run "docker compose up". All containers should be running. 

Development endpoints:
 - Web UI: https://localhost44466
 - API: https://localhost:7146/swagger
 - Dev SMTP: http://localhost:5000
 - Kibana: http://localhost:5601