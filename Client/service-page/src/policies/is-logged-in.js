module.exports = (req, res, next) => {
  console.log('asdaaaaaaaaaa')
  if (req.cookies.token)
    return next()

  return res.redirect("/login")
}
