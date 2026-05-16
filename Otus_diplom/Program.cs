using Otus_diplom;
using Otus_diplom.Data;
using Otus_diplom.Services;

var storage = new InMemoryStorage();
storage.Seed();

var reportService = new ReportService(storage);
var taskService = new TaskService(storage);
var consoleBot = new ConsoleBot(reportService, taskService, storage);

consoleBot.Run();
