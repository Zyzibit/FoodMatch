import { useState, useCallback } from "react";
import type { ZodTypeAny, ZodError } from "zod";

export function useFieldValidation(schema: ZodTypeAny) {
  const [touched, setTouched] = useState(false);
  const [error, setError] = useState<string | undefined>(undefined);
  const [value, setValue] = useState<string>("");

  const firstIssue = (e: ZodError): string =>
    e.issues[0]?.message ?? "Niepoprawna wartość";

  const validate = useCallback(
    (val: string) => {
      const res = schema.safeParse(val);
      if (res.success) {
        setError(undefined);
        return true;
      }
      setError(firstIssue(res.error));
      return false;
    },
    [schema]
  );

  const onBlur = useCallback(
    (val: string) => {
      setTouched(true);
      validate(val);
    },
    [validate]
  );

  const onChange = useCallback(
    (val: string, liveRevalidate = true) => {
      setValue(val);
      if (touched && liveRevalidate) validate(val);
    },
    [touched, validate]
  );

  return {
    value,
    setValue,
    touched,
    error,
    setError,
    onBlur,
    onChange,
    validate,
  };
}
