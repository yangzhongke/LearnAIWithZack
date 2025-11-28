1. docker run -d --name ollama -p 11434:11434 ollama/ollama
2. docker exec -it ollama ollama run llama3
3. docker exec -it ollama ollama run mxbai-embed-large
4. api base url: http://127.0.0.1:11434/v1/
5. model names: llama3, mxbai-embed-large