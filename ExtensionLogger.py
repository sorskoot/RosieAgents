class ExtensionLogger:
    def __init__(self, existingLogger) -> None:
        self.logger = existingLogger;
        pass

    def debug(self, s: str) -> None:
        self.logger.debug(self, s);
        pass

    def info(self, s: str) -> None:
        self.logger.info(self, s);
        pass

    def warning(self, s: str) -> None:
        self.logger.warning(self, s);
        pass

    def error(self, s: str) -> None:
        self.logger.error(self, s);
        pass
