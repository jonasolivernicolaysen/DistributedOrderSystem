import { useState, createContext, useContext } from "react";
interface AuthContextType {
    isLoggedIn: boolean;
    login: (token: string) => void;
    logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
    const [isLoggedIn, setIsLoggedIn] = useState(
        !!localStorage.getItem("jwt")
    );

    const login = (jwt: string, refreshToken: string) => {
        localStorage.setItem("jwt", jwt);
        localStorage.setItem("refreshToken", refreshToken);
        setIsLoggedIn(true);
    }

    const logout = () => {
        localStorage.removeItem("jwt");
        setIsLoggedIn(false)
    }

    return (
        <AuthContext.Provider value={{ isLoggedIn, login, logout }}>
            {children}
        </AuthContext.Provider>
    )
}

export function useAuth() {
    const context = useContext(AuthContext);

    if (!context) {
        throw new Error("useAuth must be used within AuthProvider")
    }

    return context;
}