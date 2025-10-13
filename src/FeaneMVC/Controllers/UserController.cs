using FeaneMVC.Application.Commands.Users;
using FeaneMVC.Application.Queries.Users;
using FeaneMVC.Contracts.Users;
using FeaneMVC.Extenstions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FeaneMVC.Controllers
{
    public class UserController : Controller
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        // GET: /User/Index
        public async Task<IActionResult> Index(Guid? editId)
        {
            // Fetch all users asynchronously
            var users = await _mediator.Send(new GetAllUsersQuery());

            UserSummary? userToEdit = null;
            if (editId.HasValue)
            {
                var userResponse = await _mediator.Send(new GetUserProfileByIdQuery(editId.Value));
                if (userResponse.Status)
                {
                    userToEdit = userResponse.Data?.User?.ToSummary();
                }
            }

            var viewModel = new UserManagementPageModel
            {
                Users = users.ToSummaryCollection(),
                UserToEdit = userToEdit
            };

            return View(viewModel);
        }

        // POST: /User/DeleteConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var deleteResponse = await _mediator.Send(new DeleteUserCommand(id));

            if (deleteResponse.Status)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "An error occurred while deleting the user.");
            var user = await _mediator.Send(new GetUserProfileByIdQuery(id));
            return View("Index", new UserManagementPageModel
            {
                Users = (await _mediator.Send(new GetAllUsersQuery())).ToSummaryCollection(),
                UserToEdit = user.Data?.User?.ToSummary()
            });
        }

        // POST: /User/UpdateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(UserManagementPageModel model)
        {
            if (model.UserToEdit == null)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating the user.");
                model.Users = (await _mediator.Send(new GetAllUsersQuery())).ToSummaryCollection();
                return View("Index", model);
            }

            var existingUserResponse = await _mediator.Send(new GetUserProfileByIdQuery(model.UserToEdit.Id));
            if (!existingUserResponse.Status || existingUserResponse.Data?.User == null)
            {
                ModelState.AddModelError(string.Empty, "Unable to load the selected user.");
                model.Users = (await _mediator.Send(new GetAllUsersQuery())).ToSummaryCollection();
                return View("Index", model);
            }

            var userToUpdate = existingUserResponse.Data.User;
            userToUpdate.Username = model.UserToEdit.Username;
            userToUpdate.Email = model.UserToEdit.Email;
            userToUpdate.Roles = model.UserToEdit.Roles;
            userToUpdate.IsActive = model.UserToEdit.IsActive;

            var updateUserResponse = await _mediator.Send(new UpdateUserCommand(userToUpdate));

            if (updateUserResponse.Status)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "An error occurred while updating the user.");
            model.Users = (await _mediator.Send(new GetAllUsersQuery())).ToSummaryCollection();
            return View("Index", model);
        }
    }
}
