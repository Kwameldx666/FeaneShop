using FeaneMVC.Application.Commands.Notifications;
using FeaneMVC.Application.Queries.Notifications;
using FeaneMVC.Contracts.Notifications;
using FeaneMVC.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MediatR;
using FeaneMVC.Application.Services;

namespace FeaneMVC.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly NotificationService _notificationService;
        private const string ChatApiKey = "sk-LKEP4dxOGU3J03YFkxdJMCihUoGBL11rTrlpIfSvprFWtdUy";
        private const string ChatBaseUrl = "https://api.gptgod.online/v1/";

        public NotificationsController(
            IMediator mediator,
            IHttpClientFactory httpClientFactory,
            NotificationService notificationService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _httpClientFactory = httpClientFactory;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var filters = await _mediator.Send(new GetNotificationFiltersQuery());
            var groupedNotifications = await ClassifyNotifications(filters);

            var model = new NotificationsPageModel
            {
                Groups = groupedNotifications.ToDictionary(
                    pair => pair.Key,
                    pair => (IReadOnlyList<NotificationItem>)pair.Value
                        .Select(notification => new NotificationItem
                        {
                            Id = notification.Id,
                            Content = notification.Content,
                            CreatedAt = notification.CreatedAt
                        })
                        .ToList()),
                Email = new EmailMessageRequest()
            };

            if (TempData.TryGetValue("EmailSuccess", out var success))
            {
                model.SuccessMessage = success as string;
            }

            if (TempData.TryGetValue("EmailError", out var error))
            {
                model.ErrorMessage = error as string;
            }

            return View(model);
        }

        public async Task<IActionResult> Filters()
        {
            var filters = await _mediator.Send(new GetNotificationFiltersQuery());
            foreach (var filter in filters)
            {
                Console.WriteLine($"Filter: {filter.Name} (ID: {filter.Id})");
            }
            return View(filters);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateFilters(string userInput)
        {
            if (string.IsNullOrEmpty(userInput))
            {
                return BadRequest("User input cannot be empty");
            }

            // Очищаем существующие фильтры
            await _mediator.Send(new ClearNotificationFiltersCommand());

            // Запрос к API GPT-3.5
            var filters = await GetFiltersFromAI(userInput);
            foreach (var filter in filters)
            {
                await _mediator.Send(new AddNotificationFilterCommand(filter));
            }

            return RedirectToAction("Filters");
        }

        [HttpPost]
        public async Task<IActionResult> AddNotification(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return BadRequest("Content cannot be empty");
            }

            await _mediator.Send(new AddNotificationCommand(content));
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmail(EmailMessageRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData["EmailError"] = "Введите корректные данные для отправки письма.";
                return RedirectToAction(nameof(Index));
            }

            await _mediator.Send(new SendEmailNotificationCommand(request.Message, request.RecipientEmail, request.Subject));
            TempData["EmailSuccess"] = "Сообщение успешно отправлено.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<string>> GetFiltersFromAI(string userInput)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ChatApiKey);

            var payload = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
          new { role = "system", content = "You are an AI that extracts keywords from user input to create filters. Analyze the input and return a plain JSON array of strings containing the most relevant keywords or their close synonyms that best represent the meaning. Avoid unnecessary words and return only the core concepts. For single-word inputs, return one keyword; for longer inputs, return multiple if applicable. Do not include additional text, code blocks, or formatting. Example: Input 'I like running and jumping' → [\"running\", \"jumping\"], Input 'I love to work hard' → [\"work\"]." },
            new { role = "user", content = userInput }
        },
                max_tokens = 100
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{ChatBaseUrl}chat/completions", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ChatResponse>(jsonResponse);
                var filtersJson = result.choices[0].message.content.Trim();

                // Удаляем возможные обратные кавычки и лишний текст
                filtersJson = filtersJson.Trim('`');
                if (filtersJson.StartsWith("json\n"))
                {
                    filtersJson = filtersJson.Substring(5); // Удаляем "json\n"
                }

                try
                {
                    return JsonSerializer.Deserialize<List<string>>(filtersJson);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine("Failed to deserialize filters: " + filtersJson);
                    Console.WriteLine("Exception: " + ex.Message);
                    return new List<string> { "Uncategorized" };
                }
            }

            return new List<string> { "Uncategorized" };
        }
        [HttpPost]
        public async Task<IActionResult> AddFilter(string filterName)
        {
            if (string.IsNullOrEmpty(filterName))
            {
                TempData["Error"] = "Filter name cannot be empty.";
                return RedirectToAction("Filters");
            }

            // Запрос к API GPT-3.5 для обработки одного фильтра
            var filters = await GetFiltersFromAI(filterName);
            if (filters == null || !filters.Any())
            {
                TempData["Error"] = "Failed to generate a filter from your input.";
                return RedirectToAction("Filters");
            }

            // Добавляем только первый фильтр (так как это одиночный ввод)
            await _mediator.Send(new AddNotificationFilterCommand(filters[0]));

            return RedirectToAction("Filters");
        }
        private async Task<Dictionary<string, List<Notification>>> ClassifyNotifications(IReadOnlyList<Filter>? filters)
        {
            var notifications = await _mediator.Send(new GetNotificationsQuery());

            if (notifications.Count == 0)
            {
                return new Dictionary<string, List<Notification>>
                {
                    { "No Notifications", new List<Notification>() }
                };
            }

            if (filters == null || filters.Count == 0)
            {
                return new Dictionary<string, List<Notification>>
                {
                    { "Uncategorized", new List<Notification>(notifications) }
                };
            }

            var groupedNotifications = new Dictionary<string, List<Notification>>();

            foreach (var filter in filters)
            {
                groupedNotifications[filter.Name] = new List<Notification>();
            }
            groupedNotifications["Uncategorized"] = new List<Notification>();

            foreach (var notification in notifications)
            {
                bool matched = false;
                foreach (var filter in filters)
                {
                    if (notification.Content.ToLower().Contains(filter.Name.ToLower()))
                    {
                        groupedNotifications[filter.Name].Add(notification);
                        matched = true;
                        break;
                    }
                }
                if (!matched)
                {
                    groupedNotifications["Uncategorized"].Add(notification);
                }
            }

            return groupedNotifications;
        }
    }

    // Классы для десериализации ответа от API
    public class ChatResponse
    {
        public ChatChoice[] choices { get; set; }
    }

    public class ChatChoice
    {
        public ChatMessage message { get; set; }
    }

    public class ChatMessage
    {
        public string content { get; set; }
    }
}