export async function apiFetch(
    url: string,
    options: RequestInit = {}
) {
    const token = localStorage.getItem("token");

    const response = await fetch(url, {
        ...options,
        headers: {
            ...options.headers,
            Authorization: `Bearer ${token}`
        }
    });

    if (response.status === 401) {
        localStorage.removeItem("token");
        window.location.href = "/login";
        return;
    }

    return response;
}