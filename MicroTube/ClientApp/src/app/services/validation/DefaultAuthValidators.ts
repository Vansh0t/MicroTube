import { Injectable } from "@angular/core";
import { AbstractControl, FormGroup, ValidationErrors, ValidatorFn } from "@angular/forms";
@Injectable({
  providedIn: "root"
})
export class DefaultAuthValidators
{
  readonly MIN_USERNAME_LENGTH = 2;
  readonly MAX_USERNAME_LENGTH = 24;
  readonly MIN_PASSWORD_LENGTH = 6;
  readonly MAX_PASSWORD_LENGTH = 32;

  letterRequiredValidator(control: AbstractControl): ValidationErrors | null
  {
    if (control == null || control.value == null)
      return null;

    const LETTER_REGEX = new RegExp(/[A-Za-z]/);
    if (!LETTER_REGEX.test(control.value))
      return { letterRequiredValidator: "Required at least 1 letter." };
    return null;
  }
  digitRequiredValidator(control: AbstractControl): ValidationErrors | null
  {
    if (control == null || control.value == null)
      return null;

    const DIGIT_REGEX = new RegExp(/\d/);
    if (!DIGIT_REGEX.test(control.value))
      return { digitRequiredValidator: "Required at least 1 digit." };
    return null;
  }
  usernameValidator(control: AbstractControl): ValidationErrors | null
  {
    if (control == null || control.value == null)
      return null;

    const REGEX = new RegExp(/[^A-Za-z\d]/);
    if (REGEX.test(control.value))
      return { usernameValidator: "Letters and numbers only." };
    return null;
  }
  buildPasswordsMatchValidator(passwordControlKey: string, passwordConfirmationControlKey: string): ValidatorFn
  {
    return (control: AbstractControl): ValidationErrors| null => 
    {
      if (control as FormGroup === undefined)
        throw new Error("PasswordsMatchValidator requires FormGroup as a control");
      const group = control as FormGroup;
      const passwordsMatch = group.get(passwordControlKey)?.value === group.get(passwordConfirmationControlKey)?.value;
      const validatorError = passwordsMatch ? null : { passwordsMatchValidator: "Passwords must match." };
      group.controls[passwordConfirmationControlKey].setErrors(validatorError);
      return validatorError;
    };
  }
  
}
