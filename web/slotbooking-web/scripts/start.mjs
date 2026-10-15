// Starts the Angular dev server on the port Aspire assigns (PORT), with the API proxy.
import { spawn } from 'node:child_process';

const port = process.env.PORT ?? '4200';
const child = spawn(
  'npx',
  ['ng', 'serve', '--host', '0.0.0.0', '--port', port, '--proxy-config', 'proxy.conf.mjs'],
  { stdio: 'inherit', shell: process.platform === 'win32' },
);

child.on('exit', (code) => process.exit(code ?? 0));
for (const signal of ['SIGINT', 'SIGTERM']) {
  process.on(signal, () => child.kill(signal));
}
