using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Login2048.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prepaid2048;

namespace Login2048.Controllers
{
    [Route("api/users")]
    public class UserController : Controller
    {
        Random randomizer = new Random(DateTime.Now.Millisecond);
        private readonly UserContext _context;
        public UserController(UserContext context) => _context = context;

        #region User
        [HttpPost("/users/auth")]
        public async Task<IActionResult> Authenticate([FromBody] User loginInfo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if(loginInfo.Username == null)
            {
                return BadRequest();
            }
            User target;
            try
            {
                target = _context.Users.First(e => e.Username == loginInfo.Username);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            if (target.PasswordHash == loginInfo.PasswordHash)
            {
                Session session = await AddRefreshSessionAsync(loginInfo.Username);
                if (session == null)
                {
                    return BadRequest(ModelState);
                }
                return Ok(session.SessionId);
            }
            return Unauthorized();
        }

        [HttpPost("/users/signup")]
        public async Task<IActionResult> Signup([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if(user.Username == null || user.PasswordHash == null)
            {
                return BadRequest();
            }
            if (UserExists(user.Username))
            {
                return new ObjectResult("UserExist") { StatusCode = 409 };
                //return Ok("UserExist");
            }

            user.Coins = 2048;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            Session session = await AddRefreshSessionAsync(user.Username);
            if(session == null)
            {
                return BadRequest(ModelState);
            }
            return Created("", session.SessionId);
        }

        [HttpPost("/users/cm/{username}")]
        public async Task<IActionResult> ChangePassword([FromRoute] string username, [FromHeader] string userSid, [FromBody] string newPasswordHash)
        {
            if(newPasswordHash == null)
            {
                return BadRequest();
            }
            User target = null;
            try
            {
                target = _context.Users.First(e => e.Username == username);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }

            bool? isSessionValid = GetSessionValidity(userSid, username);
            if(isSessionValid == null)
            {
                return BadRequest(ModelState);
            }
            if(isSessionValid == false)
            {
                return Unauthorized();
            }

            target.PasswordHash = newPasswordHash;
            await _context.SaveChangesAsync();
            Session s = await AddRefreshSessionAsync(target.Username);
            return Ok(s.SessionId);
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
        private bool UserExists(string name)
        {
            return _context.Users.Any(e => e.Username == name);
        }
        #endregion

        #region Highscores
        [HttpGet("/highscores/{mode}/{size}")]
        public IActionResult GetHighScores([FromRoute] string mode, [FromRoute] string size)
        {
            GameModes gameMode = GameModes.Normal;
            int s = 0;
            try
            {
                gameMode = (GameModes)int.Parse(mode);
                s = int.Parse(size);
            }
            catch(FormatException)
            {
                return BadRequest(mode);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<Highscore> scores = new List<Highscore>();
            foreach(Highscore hs in _context.Highscores.OrderBy(x => -x.Score))
            {
                if((GameModes)hs.Mode == gameMode && hs.Size == s)
                {
                    scores.Add(hs);
                    if(scores.Count >= 10)
                    {
                        break;
                    }
                }
            }
            return Ok(scores);
        }
        [HttpPost("/highscores/add")]
        public async Task<IActionResult> AddHighScores([FromBody] Highscore highscore, [FromHeader] string userSid)
        {
            if (highscore == null || highscore.Size == 0)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if(highscore.Mode > 5 || highscore.Mode < 0)
            {
                return BadRequest();
            }

            bool? sessionValidity = GetSessionValidity(userSid, highscore.Username);
            if (sessionValidity == null)
            {
                return BadRequest(ModelState);
            }
            if (sessionValidity == false)
            {
                return Unauthorized();
            }

            highscore.Time = DateTime.UtcNow.Ticks;

            _context.Highscores.Add(highscore);
            await _context.SaveChangesAsync();
            return Ok();
        }
        #endregion

        #region Transaction
        [HttpGet("/transactions/coins/{username}")]
        public IActionResult GetCoins([FromRoute] string username, [FromHeader] string userSid)
        {
            User target = null;
            try
            {
                target = _context.Users.First(e => e.Username == username);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            bool? isSessionValid = GetSessionValidity(userSid, username);
            if (isSessionValid == null)
            {
                return BadRequest(ModelState);
            }
            if (isSessionValid == false)
            {
                return Unauthorized();
            }
            return Ok(target.Coins);
        }

        [HttpPost("/transactions/transact/{username}")]
        public async Task<IActionResult> NewTransaction([FromRoute] string username, [FromHeader] string userSid, [FromBody] int value)
        {
            if (value < 0)
            {
                return BadRequest();
            }
            User target = null;
            try
            {
                target = _context.Users.First(e => e.Username == username);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            bool? isSessionValid = GetSessionValidity(userSid, username);
            if (isSessionValid == null)
            {
                return BadRequest(ModelState);
            }
            if (isSessionValid == false)
            {
                return Unauthorized();
            }
            if (target.Coins < value)
            {
                return new ObjectResult("InsufficientFund") { StatusCode = 402 };
            }
            target.Coins -= value;
            await _context.SaveChangesAsync();
            return Ok(target.Coins);
        }

        [HttpPost("/transactions/redeem/{username}")]
        public async Task<IActionResult> RedeemCredit([FromRoute] string username, [FromHeader] string userSid, [FromBody] string cardKey)
        {
            // Validate Input
            if (cardKey == null)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Fetch User
            User target;
            try
            {
                target = _context.Users.First(x => x.Username == username);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }

            // Authenticate
            bool? sessionValidity = GetSessionValidity(userSid, username);
            if (sessionValidity == null)
            {
                return BadRequest(ModelState);
            }
            if (sessionValidity == false)
            {
                return Unauthorized();
            }

            // Validate Card
            if (GetCardValidity(cardKey) == false)
            {
                return new ObjectResult("CardRedeemed") { StatusCode = 402 };
            }

            // Get Card Value
            CardValues value;
            try
            {
                value = PrepaidCardManager.GetValue(cardKey);
            }
            catch (ArgumentException)
            {
                return new ObjectResult("CardInvalid") { StatusCode = 402 };
            }

            // Invalidate Card
            if (await AddRedeemedCardAsync(cardKey) == false)
            {
                return BadRequest();
            }

            // Credit Account
            switch (value)
            {
                case CardValues.Five:
                    target.Coins += 5120;
                    break;
                case CardValues.Ten:
                    target.Coins += 10240;
                    break;
                case CardValues.Twenty:
                    target.Coins += 20480;
                    break;
                case CardValues.Fifty:
                    target.Coins += 51200;
                    break;
                case CardValues.Hundred:
                    target.Coins += 102400;
                    break;
            }

            await _context.SaveChangesAsync();
            return Ok(target.Coins);
        }
        #endregion

        #region Themes
        [HttpGet("/themes")]
        public IActionResult GetThemeList([FromHeader] string userSid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<ThemeTitle> themeList = new List<ThemeTitle>();
            if (string.IsNullOrEmpty(userSid) == true)
            {
                foreach (var theme in _context.Themes)
                {
                    themeList.Add(new ThemeTitle(theme));
                }
                return Ok(themeList);
            }
            else
            {
                if(SessionIdExists(userSid, out Session session) == false)
                {
                    return Unauthorized();
                }
                else
                {
                    foreach(var theme in _context.Themes.Where(x => x.Uploader.ToLower() == session.UserName.ToLower()))
                    {
                        themeList.Add(new ThemeTitle(theme));
                    }
                    return Ok(themeList);
                }
            }
        }
        [HttpGet("/themes/{id}")]
        public IActionResult GetThemeContent([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            long targetId = 0;
            try
            {
                targetId = long.Parse(id);
            }
            catch(FormatException)
            {
                return BadRequest();
            }

            Theme target = null;
            try
            {
                target = _context.Themes.First(x => x.Id == targetId);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }

            return Ok(target.Content);
        }
        [HttpPost("/themes/edit")]
        public async Task<IActionResult> EditTheme([FromBody] Theme theme, [FromHeader] string userSid)
        {
            if (theme == null)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool? sessionValidity = GetSessionValidity(userSid, theme.Uploader);
            if (sessionValidity == null)
            {
                return BadRequest(ModelState);
            }
            if (sessionValidity == false)
            {
                return Unauthorized();
            }

            try
            {
                new ThemeSerializer2048.Theme(theme.Content);
            }
            catch(ArgumentException)
            {
                return BadRequest(theme);
            }

            Theme target = null;
            if(theme.Id == 0)
            {
                _context.Themes.Add(theme);
                await _context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                try
                {
                    target = _context.Themes.First(x => x.Id == theme.Id);
                }
                catch(InvalidOperationException)
                {
                    return NotFound();
                }

                if(target.Uploader.Trim() != theme.Uploader.Trim())
                {
                    return Unauthorized();
                }

                target.Name = theme.Name;
                target.Content = theme.Content;
                await _context.SaveChangesAsync();
                return Ok();
            }
        }
        #endregion

        #region Session
        public async Task<Session> AddRefreshSessionAsync(string username)
        {
            if (!ModelState.IsValid)
            {
                return null;
            }

            if (SessionExists(username, out Session session) == true)
            {
                session.LastRefresh = DateTime.UtcNow.Ticks;
                session.SessionId = GenerateSessionId();
            }
            else
            {
                session = new Session();
                session.UserName = username;
                session.LastRefresh = DateTime.UtcNow.Ticks;
                session.SessionId = GenerateSessionId();
                _context.Sessions.Add(session);
            }
            await _context.SaveChangesAsync();
            return session;
        }
        public bool? GetSessionValidity(string sessionId, string username)
        {
            if (!ModelState.IsValid)
            {
                return null;
            }

            if (SessionExists(username, out Session session) == true)
            {
                if (session.SessionId != sessionId)
                {
                    return false;
                }
                if (DateTime.UtcNow.AddDays(-1).Ticks > session.LastRefresh)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool SessionExists(string username)
        {
            return SessionExists(username, out _);
        }
        private bool SessionExists(string username, out Session session)
        {
            Session s = null;
            try
            {
                s = _context.Sessions.First(e => e.UserName == username);
            }
            catch (InvalidOperationException)
            {
                session = s;
                return false;
            }
            session = s;
            return true;
        }
        private bool SessionIdExists(string sessionId)
        {
            return SessionIdExists(sessionId, out _);
        }
        private bool SessionIdExists(string sessionId, out Session session)
        {
            Session s = null;
            try
            {
                s = _context.Sessions.First(e => e.SessionId == sessionId);
            }
            catch (InvalidOperationException)
            {
                session = s;
                return false;
            }
            session = s;
            return true;
        }

        private string GenerateSessionId()
        {
            string sid = "";
            do
            {
                sid = Generate();
            }
            while (_context.Sessions.Any(e => e.SessionId == sid));
            return sid;

            string Generate()
            {
                StringBuilder sessionIdBuilder = new StringBuilder();
                for (int i = 0; i < 24; i++)
                {
                    char c = (char)randomizer.Next('a', 'z' + 1);
                    if (randomizer.Next(0, 2) == 0)
                    {
                        sessionIdBuilder.Append(c);
                    }
                    else
                    {
                        sessionIdBuilder.Append(char.ToUpper(c));
                    }
                }
                return sessionIdBuilder.ToString();
            }
        }
        #endregion

        #region Card
        public bool? GetCardValidity(string cardKey)
        {
            if (!ModelState.IsValid)
            {
                return null;
            }

            return !(_context.RedeemedCards.Any(x => x.CardKey == cardKey));
        }
        public async Task<bool> AddRedeemedCardAsync(string cardKey)
        {
            if (!ModelState.IsValid)
            {
                return false;
            }

            if (GetCardValidity(cardKey) == false)
            {
                return false;
            }

            _context.RedeemedCards.Add(new Card() { CardKey = cardKey });
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion
    }
}
