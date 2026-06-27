import { useState } from "react";
import type { SubmitEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../components/AuthContext"
import { toast } from "react-toastify"
import { apiFetch } from "../services/api"


function LoginPage() {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");

    const { login } = useAuth();
    const navigate = useNavigate();

    const handleLogin = async (e: SubmitEvent) => {
        e.preventDefault();
        if (hasErrors) return;

        try {
            const response = await apiFetch("https://localhost:7144/api/auth/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ username, password }),
            });

            const data = await response.json();
            if (!response.ok) {
                toast.error(data.detail);
                return;
            }

            login(data.jwtToken)
            navigate("/products");

        } catch (error) {
            console.error(error);
            toast.error("Could not connect to the server");
        }
    };

    const usernameError = username.length > 0 && username.length < 4
        ? "Must be at least 4 characters"
        : "";

    const passwordError = password.length > 0 && password.length < 8
        ? "Must be at least 8 characters"
        : "";

    const hasErrors = !!(usernameError || passwordError);

    const fieldClass = (error: string) => `form-control ${error ? "is-invalid" : ""}`;

    return (
        <div className="container mt-5">
            <div className="row justify-content-center">
                <div className="col-md-4">
                    <div className="card shadow">
                        <div className="card-body">
                            <h2 className="text-center mb-4">Login</h2>

                            <form onSubmit={handleLogin} noValidate>
                                <div className="mb-3">
                                    <input
                                        className={fieldClass(usernameError)}
                                        value={username}
                                        onChange={(e) => setUsername(e.target.value)}
                                        type="text"
                                        placeholder="Username"
                                        required
                                        autoComplete="username"
                                    />
                                    {usernameError && (
                                        <div className="invalid-feedback d-block">{usernameError}</div>
                                    )}
                                </div>

                                <div className="mb-3">
                                    <input
                                        className={fieldClass(passwordError)}
                                        value={password}
                                        onChange={(e) => setPassword(e.target.value)}
                                        type="password"
                                        placeholder="Password"
                                        required
                                        autoComplete="current-password"
                                    />
                                    {passwordError && (
                                        <div className="invalid-feedback d-block">{passwordError}</div>
                                    )}
                                </div>

                                <button
                                    type="submit"
                                    className="btn btn-primary w-100"
                                    disabled={hasErrors}
                                >
                                    Login
                                </button>
                            </form>

                            <p className="text-center text-muted small mt-3 mb-0">
                                Don't have an account? <a href="/register">Register here</a>
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default LoginPage;