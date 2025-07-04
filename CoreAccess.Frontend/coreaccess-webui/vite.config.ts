import { defineConfig, loadEnv } from 'vite';
import vue from '@vitejs/plugin-vue';

export default defineConfig(({ mode }) => {
    const env = loadEnv(mode, '.', '');

    return {
        plugins: [vue()],
        server: {
            host: true,
            port: Number(env.VITE_FRONTEND_PORT) || 5173,
        },
        base: '/',
        build: {
            outDir: 'dist',
        },
    };
});
