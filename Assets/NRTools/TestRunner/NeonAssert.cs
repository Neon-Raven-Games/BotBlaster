using System;
using System.Collections.Generic;
using UnityEngine;

namespace NRTools
{
    public static class NeonAssert
    {
        private static AssertionResult ThrownException(Exception ex, string customMessage = null)
        {
            return AssertionResult.Fail(
                $"Assertion failed with exception: {ex.Message}, inner exception: {ex.InnerException?.Message}", ex.StackTrace);
        }
        
        public static AssertionResult AreEqual<T>(T expected, T actual, string customMessage = null)
        {
            try
            {
                if (EqualityComparer<T>.Default.Equals(expected, actual))
                    return AssertionResult.Success(customMessage ?? $"Expected: {expected}, and got: {actual}");

                return AssertionResult.Fail(customMessage ?? $"Expected: {expected}, but got: {actual}");
            }
            catch (Exception ex)
            {
                return ThrownException(ex);
            }
        }

        public static AssertionResult AreNotEqual<T>(T notExpected, T actual, string customMessage = null)
        {
            try
            {
                if (!EqualityComparer<T>.Default.Equals(notExpected, actual))
                    return AssertionResult.Success(customMessage ??
                                                   $"Did not expect: {notExpected}, and got: {actual}");

                return AssertionResult.Fail(customMessage ?? $"Did not expect: {notExpected}, but got: {actual}");
            }
            catch (Exception ex)
            {
                return ThrownException(ex);
            }
        }

        public static AssertionResult GreaterThan<T>(T greaterThanValue, T actual, string customMessage = null)
            where T : IComparable<T>
        {
            try
            {
                if (greaterThanValue.CompareTo(actual) > 0)
                    return AssertionResult.Success(customMessage ??
                                                   $"Expected {actual} to be greater than {greaterThanValue}.");
                
                return AssertionResult.Fail(customMessage ??
                                            $"Expected {actual} to be greater than {greaterThanValue}, but it wasn't.");
            }
            catch (Exception ex)
            {
                return ThrownException(ex);
            }
        }

        public static AssertionResult LessThan<T>(T lessThanValue, T actual, string customMessage = null)
            where T : IComparable<T>
        {
            try
            {
                if (actual.CompareTo(lessThanValue) > 0)
                    return AssertionResult.Success(customMessage ??
                                                   $"Expected {actual} to be less than {lessThanValue}.");

                return AssertionResult.Fail(customMessage ??
                                            $"Expected {actual} to be less than {lessThanValue}, but it wasn't.");
            }
            catch (Exception ex)
            {
                return ThrownException(ex);
            }
        }

        // Custom NotActive assertion (for GameObjects)
        public static AssertionResult NotActive(GameObject gameObject, string customMessage = null)
        {
            try
            {
                if (!gameObject.activeInHierarchy)
                    return AssertionResult.Success(customMessage ?? $"{gameObject.name} is not active.");
                return AssertionResult.Fail(customMessage ??
                                            $"{gameObject.name} is active but was expected to be inactive.");
            }
            catch (Exception ex)
            {
                return AssertionResult.Fail(
                    $"Assertion failed with exception: {ex.Message}, inner exception: {ex.InnerException?.Message}", ex.StackTrace);
            }
        }

        public static AssertionResult Active(GameObject gameObject, string customMessage = null)
        {
            try
            {
                if (gameObject.activeInHierarchy)
                    return AssertionResult.Success(customMessage ?? $"{gameObject.name} is active.");
                return AssertionResult.Fail(customMessage ??
                                            $"{gameObject.name} is not active but was expected to be active.");
            }
            catch (Exception ex)
            {
                return ThrownException(ex);
            }
        }

        public static AssertionResult IsNull(object obj, string customMessage = null)
        {
            try
            {
                if (obj == null)
                    return AssertionResult.Success(customMessage ?? "Object is null as expected.");
                return AssertionResult.Fail(customMessage ?? "Object was expected to be null but was not.");
            }
            catch (Exception ex)
            {
                return ThrownException(ex);
            }
        }

        public static AssertionResult IsNotNull(object obj, string customMessage = null)
        {
            try
            {
                if (obj != null)
                    return AssertionResult.Success(customMessage ?? "Object is null as expected.");
                return AssertionResult.Fail(customMessage ?? "Object was expected to be null but was not.");
            }
            catch (Exception ex)
            {
                return ThrownException(ex);
            }
        }

        public static AssertionResult Fail(string message)
        {
            return AssertionResult.Fail(message);
        }
    }
}