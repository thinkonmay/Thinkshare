using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Signalling.Data;
using Signalling.Interfaces;
using Signalling.Models;

namespace Signalling.Controllers
{
    public class SessionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebSocketHandler _wsHandler;

<<<<<<< Updated upstream
        public SessionsController(ApplicationDbContext context, IWebSocketHandler wsHandler)
=======
        private readonly ISessionQueue Queue;

        public SessionsController(IWebSocketHandler wsHandler, ISessionQueue queue)
>>>>>>> Stashed changes
        {
            _context = context;
            _wsHandler = wsHandler;
            Queue = queue;
        }

        // POST: ManageSessions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> Create(SessionRequest req)
        {
            if (ModelState.IsValid)
            {
                _context.Add(new Session(req));
                await _context.SaveChangesAsync();

                return Ok();
            }

            return BadRequest();
        }

        // GET: ManageSessions/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> Delete(SessionRequest req)
        {
            if (req == null)
            {
<<<<<<< Updated upstream
                return NotFound();
            }
=======
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine($"Accepted connection '{context.Connection.Id}'");
                Task t = _wsHandler.Handle(webSocket);

                t.Wait();

                await _wsHandler.Close(webSocket);
>>>>>>> Stashed changes

            var session = await _context.Sessions.
                FirstOrDefaultAsync(o => o.ClientId == req.ClientId && o.SlaveId == req.SlaveId);
            if (session == null)
            {
                return NotFound();
            }

            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public async Task Pair()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                if (webSocket != null && webSocket.State == WebSocketState.Open)
                {
                    await _wsHandler.Handle(webSocket);
                }
            }
        }
    }
}
