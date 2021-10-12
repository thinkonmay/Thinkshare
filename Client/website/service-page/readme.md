# INSTALL
***Required [nodejs](https://nodejs.org/en/) v16+**
```
npm install
```
# SCRIPT
1. Normal start:
```
npm start
```
2. Start with **nodemon** (will display error stack trace in **error page**)
```
npm run dev
```
3. Debug server (use flag "--inspect-brk", visit "edge://inspect" or "chrome://inspect" for more information)
```
npm run debug
```
# CONFIG PATHS

 - Server routes: **src/routes.js**
 - Server policies: **src/policies.js**
 - API defines: **public/javascripts/util/api.js**
 - PORT & MODE: **.env**