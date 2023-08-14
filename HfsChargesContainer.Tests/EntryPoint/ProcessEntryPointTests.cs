using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HfsChargesContainer.Domain.Exceptions;
using HfsChargesContainer.UseCases.Interfaces;
using Moq;
using Xunit;

namespace HfsChargesContainer.Tests.EntryPoint;

public class ProcessEntryPointTests
{
    private ProcessEntryPoint _classUnderTest;
    private Mock<ILoadChargesUseCase> _loadChargesUCMock;
    private Mock<ILoadChargesHistoryUseCase> _loadChargesHistoryUCMock;
    private Mock<ICheckChargesBatchYearsUseCase> _checkChargesBatchYearsUCMock;
    private Mock<ILoadChargesTransactionsUseCase> _loadChargesTransactionsUCMock;

    public ProcessEntryPointTests()
    {
        _loadChargesUCMock = new Mock<ILoadChargesUseCase>();
        _loadChargesHistoryUCMock = new Mock<ILoadChargesHistoryUseCase>();
        _checkChargesBatchYearsUCMock = new Mock<ICheckChargesBatchYearsUseCase>();
        _loadChargesTransactionsUCMock = new Mock<ILoadChargesTransactionsUseCase>();

        _classUnderTest = new ProcessEntryPoint(
            _loadChargesUCMock.Object,
            _loadChargesHistoryUCMock.Object,
            _checkChargesBatchYearsUCMock.Object,
            _loadChargesTransactionsUCMock.Object
        );
    }

    [Fact]
    public async Task WhenThereAreNoFinancialYearsToProcessThenTheProcessDoesNotProceedToChargesIngest()
    {
        // arrange
        _checkChargesBatchYearsUCMock.Setup(u => u.ExecuteAsync()).ReturnsAsync(false);

        // act
        await _classUnderTest.Run();

        // assert
        _loadChargesUCMock.Verify(u => u.ExecuteAsync(), Times.Never);
    }

    [Fact]
    public async Task WhenTheChargesIngestGetsTriggeredAndSourceSheetIsFoundThenAllIngestSubstepsGetExecuted()
    {
        // arrage
        var noYearsToProcess = 1;
        var financialYearsQueue = GetFinancialYearsQueue(noYearsToProcess);

        _checkChargesBatchYearsUCMock
            .Setup(u => u.ExecuteAsync())
            .ReturnsAsync(ProcessedYearCheckCallback(financialYearsQueue));

        _loadChargesUCMock.Setup(u => u.ExecuteAsync()).ReturnsAsync(true);
        _loadChargesHistoryUCMock.Setup(u => u.ExecuteAsync());
        _loadChargesTransactionsUCMock.Setup(u => u.ExecuteAsync());

        // act
        await _classUnderTest.Run();

        // assert
        _checkChargesBatchYearsUCMock.Verify(u => u.ExecuteAsync(), Times.Exactly(2));

        _loadChargesUCMock.Verify(u => u.ExecuteAsync(), Times.Once);
        _loadChargesHistoryUCMock.Verify(u => u.ExecuteAsync(), Times.Once);
        _loadChargesTransactionsUCMock.Verify(u => u.ExecuteAsync(), Times.Once);
    }

    [Fact]
    public async Task WhenTheLoadChargesDoesNotFindASourceSheetThenTheRestOfTheChargesIngestProcessDoesNotExecute()
    {
        // arrange
        _checkChargesBatchYearsUCMock.Setup(u => u.ExecuteAsync()).ReturnsAsync(true);
        _loadChargesUCMock.Setup(u => u.ExecuteAsync()).ReturnsAsync(false);

        // act
        await PreventExceptionBubble(async () => await _classUnderTest.Run());

        // assert
        _checkChargesBatchYearsUCMock.Verify(u => u.ExecuteAsync(), Times.Once);

        _loadChargesUCMock.Verify(u => u.ExecuteAsync(), Times.Once);
        _loadChargesHistoryUCMock.Verify(u => u.ExecuteAsync(), Times.Never);
        _loadChargesTransactionsUCMock.Verify(u => u.ExecuteAsync(), Times.Never);
    }

    [Fact]
    public async Task WhenTheLoadChargesDoesNotFindASourceSheetThenEntryPointThrows()
    {
        // arrange
        _checkChargesBatchYearsUCMock.Setup(u => u.ExecuteAsync()).ReturnsAsync(true);
        _loadChargesUCMock.Setup(u => u.ExecuteAsync()).ReturnsAsync(false);

        // act
        Func<Task> processRun = async () => await _classUnderTest.Run();

        // assert
        await processRun
            .Should()
            .ThrowAsync<ResourceCannotBeFoundException>(because: "GSheet Data source is not found")
            .WithMessage("GDrive charges data sheets' identifiers were not found!");
    }

    [Fact]
    public async Task WhenMultipleFinancialYearsNeedToBeProcessedThenIngestProcessGetsTriggeredForEachOfThem()
    {
        // arrange
        var noYearsToProcess = 5;
        var financialYearsQueue = GetFinancialYearsQueue(noYearsToProcess);

        _checkChargesBatchYearsUCMock
            .Setup(u => u.ExecuteAsync())
            .ReturnsAsync(ProcessedYearCheckCallback(financialYearsQueue));

        _loadChargesUCMock.Setup(u => u.ExecuteAsync()).ReturnsAsync(true);

        // act
        await _classUnderTest.Run();

        // assert
        _checkChargesBatchYearsUCMock.Verify(u => u.ExecuteAsync(), Times.Exactly(noYearsToProcess + 1));

        _loadChargesUCMock.Verify(u => u.ExecuteAsync(), Times.Exactly(noYearsToProcess));
        _loadChargesHistoryUCMock.Verify(u => u.ExecuteAsync(), Times.Exactly(noYearsToProcess));
        _loadChargesTransactionsUCMock.Verify(u => u.ExecuteAsync(), Times.Exactly(noYearsToProcess));
    }

    [Fact]
    public async Task WhenChargesIngestFailsForAnyFinancialYearThenTheSubsequentYearsDoNotGetProcessed()
    {
        // arrange
        var noYearsToProcess = 3;
        var financialYearsQueue = GetFinancialYearsQueue(noYearsToProcess);

        _checkChargesBatchYearsUCMock
            .Setup(u => u.ExecuteAsync())
            .ReturnsAsync(ProcessedYearCheckCallback(financialYearsQueue));

        _loadChargesUCMock.Setup(u => u.ExecuteAsync()).ReturnsAsync(true);

        // Throw when processing 2nd year
        Action action = () => { if (financialYearsQueue.Count == 1) throw new Exception("Unknown error."); };

        _loadChargesHistoryUCMock
            .Setup(u => u.ExecuteAsync())
            .Callback(action);

        // act
        await PreventExceptionBubble(async () => await _classUnderTest.Run());

        // assert

        // Received a call for the 1st successful year & a call while processing 2nd failed year, but no call for the 3rd unprocessed year.
        _checkChargesBatchYearsUCMock.Verify(u => u.ExecuteAsync(), Times.Exactly(2));
        _loadChargesUCMock.Verify(u => u.ExecuteAsync(), Times.Exactly(2));
        _loadChargesHistoryUCMock.Verify(u => u.ExecuteAsync(), Times.Exactly(2));

        // Received a call for the 1st successful year, but received no call for the 2nd failed year or 3rd unprocessed year.
        _loadChargesTransactionsUCMock.Verify(u => u.ExecuteAsync(), Times.Exactly(1));
    }

    #region Common Functions
    private Queue<int> GetFinancialYearsQueue(int yearsCount, int customStart = 2020)
        => new Queue<int>(Enumerable.Range(customStart, yearsCount));

    private Func<bool> ProcessedYearCheckCallback(Queue<int> financialYearQueue)
    {
        return () =>
        {
            var isThereYearToProcess = financialYearQueue.Count > 0;

            if (isThereYearToProcess)
                financialYearQueue.Dequeue();

            return isThereYearToProcess;
        };
    }

    private async Task PreventExceptionBubble(Func<Task> callback)
    {
        try
        {
            await callback.Invoke();
        }
        catch
        {
            // We're expecting the ^^^ to fail, so adding this Try-Catch to prevent
            // the exception from bubbling to the test & making it exit before the assertions
        }
    }
    #endregion
}
