using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;

namespace EgoEngineLibrary.Frontend.ViewModels;

/// <summary>
/// A base class for objects implementing the <see cref="INotifyDataErrorInfo"/> interface. This class
/// also inherits from <see cref="ViewModelBase"/>, so it can be used for observable items too.
/// </summary>
/// <remarks>
/// Based on ObservableValidator in CommunityToolkit.Mvvm 4.2.1.
/// </remarks>
public abstract class ValidatableViewModelBase<TSelf> : ViewModelBase, INotifyDataErrorInfo
    where TSelf : ValidatableViewModelBase<TSelf>
{
    /// <summary>
    /// The <see cref="Dictionary{TKey,TValue}"/> instance used to store previous validation results.
    /// </summary>
    private readonly Dictionary<string, List<ValidationFailure>> _errors = new();

    /// <summary>
    /// Indicates the total number of properties with errors (not total errors).
    /// This is used to allow <see cref="HasErrors"/> to operate in O(1) time, as it can just
    /// check whether this value is not 0 instead of having to traverse <see cref="_errors"/>.
    /// </summary>
    private int _totalErrors;

    /// <inheritdoc />
    public bool HasErrors => _totalErrors > 0;

    /// <inheritdoc />
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <summary>
    /// The validator for the object.
    /// </summary>
    protected abstract IValidator<TSelf> Validator { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ValidatableViewModelBase{TSelf}"/>.
    /// </summary>
    protected ValidatableViewModelBase()
    {
    }

    /// <summary>
    /// Clears the validation errors for a specified property or for the entire entity.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property to clear validation errors for.
    /// If a <see langword="null"/> or empty name is used, all entity-level errors will be cleared.
    /// </param>
    protected void ClearErrors(string? propertyName = null)
    {
        // Clear entity-level errors when the target property is null or empty
        if (string.IsNullOrEmpty(propertyName))
        {
            ClearAllErrors();
        }
        else
        {
            ClearErrorsForProperty(propertyName);
        }
    }

    /// <inheritdoc cref="INotifyDataErrorInfo.GetErrors(string)"/>
    public IEnumerable<ValidationFailure> GetErrors(string? propertyName = null)
    {
        // Get entity-level errors when the target property is null or empty
        if (string.IsNullOrEmpty(propertyName))
        {
            // Local function to gather all the entity-level errors
            [MethodImpl(MethodImplOptions.NoInlining)]
            IEnumerable<ValidationFailure> GetAllErrors()
            {
                return _errors.Values.SelectMany(static errors => errors);
            }

            return GetAllErrors();
        }

        // Property-level errors, if any
        if (_errors.TryGetValue(propertyName, out var errors))
        {
            return errors;
        }

        // The INotifyDataErrorInfo.GetErrors method doesn't specify exactly what to
        // return when the input property name is invalid, but given that the return
        // type is marked as a non-nullable reference type, here we're returning an
        // empty array to respect the contract. This also matches the behavior of
        // this method whenever errors for a valid properties are retrieved.
        return [];
    }

    /// <inheritdoc />
    IEnumerable INotifyDataErrorInfo.GetErrors(string? propertyName) => GetErrors(propertyName);

    /// <summary>
    /// Validates all the properties in the current instance and updates all the tracked errors.
    /// If any changes are detected, the <see cref="ErrorsChanged"/> event will be raised.
    /// </summary>
    protected void ValidateAllProperties()
    {
        var result = Validator.Validate((TSelf)this);
        if (result.IsValid)
        {
            ClearAllErrors();
            return;
        }

        foreach (var propertyGroup in result.Errors.GroupBy(x => x.PropertyName))
        {
            SetErrors(propertyGroup.Key, propertyGroup);
        }
    }

    /// <summary>
    /// Validates a property with a specified name.
    /// If any changes are detected, the <see cref="ErrorsChanged"/> event will be raised.
    /// </summary>
    /// <param name="propertyName">The name of the property to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyName"/> is <see langword="null"/>.</exception>
    protected void ValidateProperty([CallerMemberName] string propertyName = null!)
    {
        ArgumentNullException.ThrowIfNull(propertyName);
        var result = Validator.Validate((TSelf)this, options => options.IncludeProperties(propertyName));
        if (result.IsValid)
        {
            ClearErrorsForProperty(propertyName);
            return;
        }

        foreach (var propertyGroup in result.Errors.GroupBy(x => x.PropertyName))
        {
            SetErrors(propertyGroup.Key, propertyGroup);
        }
    }

    /// <summary>
    /// Validates a property with a specified name.
    /// If any changes are detected, the <see cref="ErrorsChanged"/> event will be raised.
    /// </summary>
    /// <param name="propertyExpression">The expression of the property to validate.</param>
    protected void ValidateProperty(Expression<Func<TSelf, object>> propertyExpression)
    {
        var result = Validator.Validate((TSelf)this, options => options.IncludeProperties(propertyExpression));
        if (result.IsValid)
        {
            ClearErrorsForProperty(MemberNameValidatorSelector.MemberNamesFromExpressions(propertyExpression)[0]);
            return;
        }

        foreach (var propertyGroup in result.Errors.GroupBy(x => x.PropertyName))
        {
            SetErrors(propertyGroup.Key, propertyGroup);
        }
    }

    private void SetErrors(string propertyName, IEnumerable<ValidationFailure> errors)
    {
        // Check if the property had already been previously validated, and if so retrieve
        // the reusable list of validation errors from the errors dictionary. This list is
        // used to add new validation errors below, if any are produced by the validator.
        // If the property isn't present in the dictionary, add it now to avoid allocations.
        if (!_errors.TryGetValue(propertyName, out var propertyErrors))
        {
            propertyErrors = [];
            _errors.Add(propertyName, propertyErrors);
        }

        // Clear the errors for the specified property, if any
        var errorsChanged = false;
        if (propertyErrors.Count > 0)
        {
            propertyErrors.Clear();
            errorsChanged = true;
        }

        // Set the new errors
        propertyErrors.AddRange(errors);
        var isValid = propertyErrors.Count == 0;

        // Update the shared counter for the number of errors, and raise the
        // property changed event if necessary. We decrement the number of total
        // errors if the current property is valid but it wasn't so before this
        // validation, and we increment it if the validation failed after being
        // correct before. The property changed event is raised whenever the
        // number of total errors is either decremented to 0, or incremented to 1.
        if (isValid)
        {
            if (errorsChanged)
            {
                _totalErrors--;
                if (_totalErrors == 0)
                {
                    OnPropertyChanged(ValidatableViewModelBaseStatic.HasErrorsChangedEventArgs);
                }
            }
        }
        else if (!errorsChanged)
        {
            _totalErrors++;
            if (_totalErrors == 1)
            {
                OnPropertyChanged(ValidatableViewModelBaseStatic.HasErrorsChangedEventArgs);
            }
        }

        // Only raise the event once if needed. This happens either when the target property
        // had existing errors and is now valid, or if the validation has failed and there are
        // new errors to broadcast, regardless of the previous validation state for the property.
        if (errorsChanged || !isValid)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Clears all the current errors for the entire entity.
    /// </summary>
    private void ClearAllErrors()
    {
        if (_totalErrors == 0)
        {
            return;
        }

        // Clear the errors for all properties with at least one error, and raise the
        // ErrorsChanged event for those properties. Other properties will be ignored.
        foreach (var propertyInfo in _errors)
        {
            var hasErrors = propertyInfo.Value.Count > 0;

            propertyInfo.Value.Clear();

            if (hasErrors)
            {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyInfo.Key));
            }
        }

        _totalErrors = 0;

        OnPropertyChanged(ValidatableViewModelBaseStatic.HasErrorsChangedEventArgs);
    }

    /// <summary>
    /// Clears all the current errors for a target property.
    /// </summary>
    /// <param name="propertyName">The name of the property to clear errors for.</param>
    private void ClearErrorsForProperty(string propertyName)
    {
        if (!_errors.TryGetValue(propertyName, out var propertyErrors) ||
            propertyErrors.Count == 0)
        {
            return;
        }

        propertyErrors.Clear();

        _totalErrors--;

        if (_totalErrors == 0)
        {
            OnPropertyChanged(ValidatableViewModelBaseStatic.HasErrorsChangedEventArgs);
        }

        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }
}

file static class ValidatableViewModelBaseStatic
{
    /// <summary>
    /// The cached <see cref="PropertyChangedEventArgs"/> for <see cref="ValidatableViewModelBase{TSelf}.HasErrors"/>.
    /// </summary>
    public static readonly PropertyChangedEventArgs HasErrorsChangedEventArgs =
        new(nameof(ValidatableViewModelBase<>.HasErrors));
}
