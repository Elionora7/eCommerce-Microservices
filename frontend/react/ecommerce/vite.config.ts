import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: [
      {
        find: '@',
        replacement: path.resolve(__dirname, 'src')
      }
    ]
  },
  server: {
    port: 5173,
    strictPort: true,
    hmr: {
      overlay: false // Disable HMR overlay if it's causing issues
    }
  },
  optimizeDeps: {
    include: [
      'react',
      'react-dom',
      'react-router-dom',
      '@heroicons/react' ,
      'zod'
    ],
    exclude: ['js-big-decimal'] // Exclude problematic packages if needed
  },
  build: {
    rollupOptions: {
      external: [],
      output: {
        manualChunks: {
          react: ['react', 'react-dom'],
          router: ['react-router-dom'],
          vendor: ['axios', 'zod'] // Group other vendor dependencies
        }
      }
    },
    chunkSizeWarningLimit: 1000 // Adjust based on your needs
  },
  css: {
    postcss: './postcss.config.cjs',
    modules: {
      localsConvention: 'camelCase' // Better CSS module handling
    }
  }
})