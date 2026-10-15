// Dev-server proxy: forwards /api to the ASP.NET Core API (default Kestrel HTTP port).
export default {
  '/api': {
    target: process.env.API_URL ?? 'http://localhost:5080',
    secure: false,
    changeOrigin: true,
  },
};
