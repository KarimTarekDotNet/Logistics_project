import tailwindcss from "@tailwindcss/vite";
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    host: '0.0.0.0',
    proxy: {
      '/api': {
        target: 'https://localhost:7100',
        changeOrigin: true,
        secure: false
      },
      '/shipments': {
        target: 'https://localhost:7100',
        changeOrigin: true,
        secure: false
      }
    },
    allowedHosts: [
      'ingrainedly-hyperdemocratic-joleen.ngrok-free.dev'
    ]
  }
})
