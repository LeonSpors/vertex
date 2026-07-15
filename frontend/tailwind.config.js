/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        ink: '#08111f',
        panel: '#0e1a2c',
        line: '#1f3048',
        accent: '#63e6be',
        'accent-soft': '#16453e',
        violet: '#9d8cff',
      },
      boxShadow: {
        glow: '0 0 0 1px rgba(99,230,190,.08), 0 20px 60px rgba(0,0,0,.22)',
      },
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui', 'sans-serif'],
        mono: ['"JetBrains Mono"', 'ui-monospace', 'SFMono-Regular', 'monospace'],
      },
    },
  },
  plugins: [],
}

