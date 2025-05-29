export default {
    content: [
        "./index.html",
        "./src/**/*.{vue,js,ts,jsx,tsx}",
        "./node_modules/primevue/**/*.{js,ts,vue}", // damit Tailwind auch PrimeVue scannt
    ],
    darkMode: 'class',
    theme: {
        extend: {
            colors: {
                'gray-850': '#1e1e2f',
            },
        },
    },
    plugins: [],
}