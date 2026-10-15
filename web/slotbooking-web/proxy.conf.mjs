// Dev-server proxy: forwards /api to the API project. Aspire injects the API address
// into the frontend's environment through WithReference(api).
const target =
  process.env.services__api__http__0 ??
  process.env.services__api__https__0 ??
  'http://localhost:5000';

export default {
  '/api': {
    target,
    secure: false,
    changeOrigin: true,
  },
};
