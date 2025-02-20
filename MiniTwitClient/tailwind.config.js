/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./**/*.razor",
        "./Pages/**/*.razor",   // Corrected this line
        "./Shared/**/*.razor",
        "./wwwroot/**/*.html"
    ],
    theme: {
        extend: {},
    },
    plugins: [],
}
