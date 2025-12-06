import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true, 
    host: true, 
    proxy: {
      '/api': {
        target: process.env.VITE_API_BASE_URL || 'https://localhost:7257',
        changeOrigin: true,
        secure: false,
      }
    }
  }
})
