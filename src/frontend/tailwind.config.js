/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}", // Scan all .html and .ts files within the src directory
  ],
  theme: {
    extend: {
      colors: {
        primary: "#84ad28", // Your main color
        secondary: "#004d93", // Your secondary color
      },
      accentColor: {
        primary: "var(--color-primary)",
        secondary: "var(--color-secondary)",
      },
    },
  },
  plugins: [require("tailwind-scrollbar")],
};
