module.exports = function unauthorized() {

  const req = this.req
  const res = this.res

  if (req.wantsJSON) {
    return res.sendStatus(401)
  }
  // Or log them out (if necessary) and then redirect to the login page.
  else {

    if (req.session.userId) {
      delete req.session.userId
    }

    return res.redirect('/login')
  }

}
