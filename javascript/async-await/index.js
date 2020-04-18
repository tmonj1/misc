const path = require('path');

// _print - _print message with counter
//
// example:
//   _print('message1'); -> '0: message1'
//   _print('message2'); -> '1: message2'
//
const _print = (() => {
  let counter = 0;
  return (message) => {
    console.log(`${counter}: ${message}`);
    counter++;
  };
})();
    
// create a promise
function _createPromise() {
  return new Promise((resolve, reject) => {
    setTimeout(()=> {
      resolve();
      _print('promise resolved');
    }, 100);
    _print('promise created');
  });
}

// case 1: normal async method call
function asyncMethod() {
  setTimeout(()=> _print('asyc callback called'), 100);
}

// case 2: use promise without await
function usePromise() {
  _print('before promise');
  _createPromise().then(() => _print('promise then called'));
  _print('after promise');
}

// case 3: use promise with await
async function usePromiseWithAwait() {
  _print('before promise');
  await _createPromise().then(() => _print('promise then called'));
  _print('after promise');
}

// case 4: mimic async using generator
function* usePromiseWithGenerator(thenFunc) {
  _print('before promise');
  yield _createPromise().then(() => _print('promise then called')).then(thenFunc);
  _print('after promise');
}

//
// main
//

// check arguments
const callTypes = ['-h', 'async', 'promise', 'promise-await', 'generator'];
if (process.argv.length != 3 || !callTypes.includes(process.argv[2]) || process.argv[2] == '-h') {
  console.error(`usage: ${path.basename(process.argv[0])} ${path.basename(__filename)} ${callTypes.join('|')}`);
  process.exit(-1);
}
const callType = process.argv[2];

// call methods
_print('main started');

switch (callType) {
  case 'async':
    asyncMethod();
    break;
  case 'promise':
    usePromise();
    break;
  case 'promise-await':
    usePromiseWithAwait();
    break;
  case 'generator':
    const g = usePromiseWithGenerator(()=> {
      g.next();
    });
    g.next();
    break;
  default:
    console.error('arguement error.');
    break;
}

_print('main finished');
