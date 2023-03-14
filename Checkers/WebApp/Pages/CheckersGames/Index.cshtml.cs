using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.CheckersGames
{
    public class IndexModel : PageModel
    {
        private readonly IGameRepository _repo;

        public IndexModel(IGameRepository repo)
        {
            _repo = repo;
        }

        public IList<CheckersGame> CheckersGame { get;set; } = default!;

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? OrderBy { get; set; }
        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(Search))
            {
                Search = Search.Trim().ToUpper();
            }

            CheckersGame = _repo.GetAllGames(Search);
        }
    }
}
