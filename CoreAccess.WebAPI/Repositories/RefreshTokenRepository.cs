using CoreAccess.WebAPI.DbContext;
using CoreAccess.WebAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace CoreAccess.WebAPI.Repositories;

public interface IRefreshTokenRepository
{

     Task<List<RefreshToken>> GetAllRefreshTokenAsync(CancellationToken cancellationToken = default);
     Task<RefreshToken> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
     Task<string> GetUserIdByRefreshToken(string token, CancellationToken cancellationToken = default);
     Task<RefreshToken> UpdateOrInsertRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
     Task DeleteRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
     Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class RefreshTokenRepository(CoreAccessDbContext context) : IRefreshTokenRepository
{
     public async Task<List<RefreshToken>> GetAllRefreshTokenAsync(CancellationToken cancellationToken = default)
     {
          return await context.Set<RefreshToken>()
               .ToListAsync(cancellationToken);
     }

     public async Task<RefreshToken> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
     {
          if (string.IsNullOrEmpty(token))
          {
               throw new ArgumentNullException(nameof(token), "Token cannot be null or empty");
          }

          var refreshToken = await context.Set<RefreshToken>()
               .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

          if (refreshToken == null)
          {
               throw new KeyNotFoundException("Refresh token not found.");
          }

          return refreshToken;
     }

     public async Task<string> GetUserIdByRefreshToken(string token, CancellationToken cancellationToken = default)
     {
          if (string.IsNullOrEmpty(token))
          {
               throw new ArgumentNullException(nameof(token), "Token cannot be null or empty");
          }

          var refreshToken = await context.Set<RefreshToken>()
               .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

          if (refreshToken == null)
          {
               throw new KeyNotFoundException("Refresh token not found.");
          }

          return refreshToken.CoreUserId.ToString();
     }

     public async Task<RefreshToken> UpdateOrInsertRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
     {
          var existingToken = await context.Set<RefreshToken>().FirstOrDefaultAsync(t => t.Token == refreshToken.Token, cancellationToken);
        
          if (existingToken == null)
          {
               await context.Set<RefreshToken>().AddAsync(refreshToken, cancellationToken);
               return refreshToken;
          }

          context.Entry(existingToken).CurrentValues.SetValues(refreshToken);
          context.Entry(existingToken).State = EntityState.Modified;
          return existingToken;
     }

     public async Task DeleteRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
     {
          if (string.IsNullOrEmpty(token))
          {
               throw new ArgumentNullException(nameof(token), "Token cannot be null or empty");
          }

          var refreshToken = await context.Set<RefreshToken>().FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
          if (refreshToken == null)
          {
               throw new KeyNotFoundException("Refresh token not found.");
          }

          context.Set<RefreshToken>().Remove(refreshToken);
     }

     public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
     {
          await context.SaveChangesAsync(cancellationToken);
     }
}