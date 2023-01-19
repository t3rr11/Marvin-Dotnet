import * as Log from './log.handler';

export const ErrorHandler = (severity, error, noTrace?) => {
  if (severity === 'High') {
    console.log(
      '\x1b[1;31m%s\x1b[0m',
      `Severity: ${severity}, Trace: ${noTrace ? 'NoTrace' : console.trace()}, Error: ${JSON.stringify(error)}`
    );
  } else if (severity === 'Med') {
    console.log(
      '\x1b[1;33m%s\x1b[0m',
      `Severity: ${severity}, Trace: ${noTrace ? 'NoTrace' : console.trace()}, Error: ${JSON.stringify(error)}`
    );
  } else if (severity === 'Low') {
    console.log(
      '\x1b[1;33m%s\x1b[0m',
      `Severity: ${severity}, Trace: ${noTrace ? 'NoTrace' : console.trace()}, Error: ${JSON.stringify(error)}`
    );
  } else {
    console.log(
      `Severity: ${severity}, Trace: ${noTrace ? 'NoTrace' : console.trace()}, Error: ${JSON.stringify(error)}`
    );
  }
  Log.SaveLog(
    'ErrorHandler',
    'Error',
    `Severity: ${severity}, Trace: ${noTrace ? 'NoTrace' : console.trace()}, Error: ${JSON.stringify(error)}`
  );
};
