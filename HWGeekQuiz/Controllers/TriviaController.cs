﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Description;
using HWGeekQuiz.Models;






namespace HWGeekQuiz.Controllers
{
    using GeekQuiz.Models;

    [Authorize]
    public class TriviaController : ApiController
    {
        private TriviaContext db = new TriviaContext();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.db.Dispose();
            }
            base.Dispose(disposing);
        }

        private async Task<TriviaQuestion> NextQuestionAsync(string userId)
        {
            var lastQuestionId = await this.db.TriviaAnswers.Where(a => a.UserId == userId).GroupBy(a => a.QuestionId)
                                     .Select(g => new { QuestionId = g.Key, Count = g.Count() })
                                     .OrderByDescending( q => new { q.Count, QuestionId = q.QuestionId })
                                     .Select(q => q.QuestionId)
                                     .FirstOrDefaultAsync();

            var questionsCount = await this.db.TriviaQuestions.CountAsync();

            var nextQuestionId = (lastQuestionId % questionsCount) + 1;
            return await this.db.TriviaQuestions.FindAsync(CancellationToken.None, nextQuestionId);

        }

        //
        // GET api/TriviaAdd the following Get action method to the TriviaController class. This action method calls the NextQuestionAsync helper method defined in the previous step to retrieve the next question for the authenticated user.
        [ResponseType((typeof(TriviaQuestion)))]
   public async Task<IHttpActionResult> Get()
    {
        var userId = User.Identity.Name;

        TriviaQuestion nextQuestion = await this.NextQuestionAsync(userId);
        if (nextQuestion == null)
        {
            return this.NotFound();
        }
        return this.Ok(nextQuestion);
    }

        //Add the following helper method at the end of the TriviaController class. This method stores the specified answer in the database and returns a Boolean value indicating whether or not the answer is correct.
        private async Task<bool> StoreAsync(TriviaAnswer answer)
        {
            this.db.TriviaAnswers.Add(answer);
            await this.db.SaveChangesAsync();
            var selectedOption =
                await this.db.TriviaOptions.FirstOrDefaultAsync(
                    o => o.Id == answer.OptionId && o.QuestionId == answer.QuestionId);
            return selectedOption.IsCorrect;
        }
        //12. Add the following Post action method to the TriviaController class. This action method associates the answer to the authenticated user and calls the StoreAsync helper method. Then, it sends a response with the Boolean value returned by the helper method.

        //POST api/Trivia
        [ResponseType(typeof(TriviaAnswer))]
        public async Task<IHttpActionResult> Post(TriviaAnswer answer)
        {
            if (!ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            answer.UserId = User.Identity.Name;

            var isCorrect = await this.StoreAsync((answer));
            return this.Ok<bool>(isCorrect);
        }



    }
}
