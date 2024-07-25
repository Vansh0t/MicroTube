FROM node:alpine
WORKDIR /app
COPY . .
RUN npm --max_old_space_size=8192 install
ENTRYPOINT npm start
